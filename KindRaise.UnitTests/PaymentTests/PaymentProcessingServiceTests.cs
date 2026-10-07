using KindRaise.Application.Donations;
using KindRaise.Application.Messaging;
using KindRaise.Application.Payments;
using KindRaise.Application.Services;
using KindRaise.Domain.Donation;
using Moq;

namespace KindRaise.UnitTests.PaymentTests
{
    public sealed class PaymentProcessingServiceTests
    {
        private readonly Mock<IDonationRepository> _donationRepository;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IPaymentProvider> _paymentProvider;
        private readonly Mock<IMessagePublisher> _messagePublisher;

        private readonly PaymentProcessingService _service;

        public PaymentProcessingServiceTests()
        {
            _donationRepository = new Mock<IDonationRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _paymentProvider = new Mock<IPaymentProvider>();
            _messagePublisher = new Mock<IMessagePublisher>();

            _service = new PaymentProcessingService(
                _donationRepository.Object,
                _unitOfWork.Object,
                _paymentProvider.Object,
                _messagePublisher.Object);
        }

        [Fact]
        public async Task ProcessAsync_WhenPaymentSucceeds_MarksDonationAsProcessed()
        {
            // Arrange
            var campaignId = Guid.NewGuid();

            var donation = new Donation(campaignId, "John Doe", 25m);

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            _paymentProvider
                .Setup(provider =>
                    provider.ProcessPaymentAsync(
                        donation.Id,
                        donation.CampaignId,
                        donation.Amount,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentResult(
                    PaymentResultStatus.Success));

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            Assert.Equal(ProcessingState.Processed, donation.ProcessingState);
        }

        [Fact]
        public async Task ProcessAsync_WhenPaymentSucceeds_SendsCorrectDonationDataToProvider()
        {
            // Arrange
            var campaignId = Guid.NewGuid();

            var donation = new Donation(campaignId, "John Doe", 42.50m);

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            _paymentProvider
                .Setup(provider =>
                    provider.ProcessPaymentAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<decimal>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PaymentResult(
                        PaymentResultStatus.Success));

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            _paymentProvider.Verify(
                provider =>
                    provider.ProcessPaymentAsync(
                        donation.Id,
                        donation.CampaignId,
                        donation.Amount,
                        It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_WhenPaymentIsDeclined_MarksDonationAsRejected()
        {
            // Arrange
            var donation = new Donation(Guid.NewGuid(), "John Doe", 25m);

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            _paymentProvider
                .Setup(provider =>
                    provider.ProcessPaymentAsync(
                        donation.Id,
                        donation.CampaignId,
                        donation.Amount,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PaymentResult(
                        PaymentResultStatus.Declined,
                        "Insufficient funds"));

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            Assert.Equal(ProcessingState.Rejected, donation.ProcessingState);
        }

        [Fact]
        public async Task ProcessAsync_WhenPaymentTemporarilyFails_MarksDonationAsTemporaryFailure()
        {
            // Arrange
            var donation = new Donation(Guid.NewGuid(), "John Doe", 25m);

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            _paymentProvider
                .Setup(provider =>
                    provider.ProcessPaymentAsync(
                        donation.Id,
                        donation.CampaignId,
                        donation.Amount,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PaymentResult(
                        PaymentResultStatus.TemporaryFailure,
                        "Provider unavailable"));

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            Assert.Equal(ProcessingState.TemporaryFailure, donation.ProcessingState);
        }

        [Fact]
        public async Task ProcessAsync_WhenDonationDoesNotExist_ThrowsInvalidOperationException()
        {
            // Arrange
            var donationId = Guid.NewGuid();

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync((Donation?)null);

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.ProcessAsync(
                    donationId,
                    CancellationToken.None));

            // Assert
            Assert.Contains(donationId.ToString(), exception.Message);

            _paymentProvider.Verify(
                provider =>
                    provider.ProcessPaymentAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<decimal>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenDonationIsAlreadyProcessed_DoesNotProcessPaymentAgain()
        {
            // Arrange
            var donation = new Donation(Guid.NewGuid(),"John Doe", 25m);

            donation.StartProcessing();
            donation.MarkAsProcessed();

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            Assert.Equal(ProcessingState.Processed, donation.ProcessingState);

            _paymentProvider.Verify(
                provider =>
                    provider.ProcessPaymentAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<decimal>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);

            _unitOfWork.Verify(
                unitOfWork =>
                    unitOfWork.SaveChangesAsync(
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(ProcessingState.Rejected)]
        [InlineData(ProcessingState.TemporaryFailure)]
        public async Task ProcessAsync_WhenDonationIsAlreadyTerminal_DoesNotProcessPaymentAgain(ProcessingState terminalState)
        {
            // Arrange
            var donation = new Donation(Guid.NewGuid(), "John Doe", 25m);

            donation.StartProcessing();

            switch (terminalState)
            {
                case ProcessingState.Rejected:
                    donation.MarkAsRejected();
                    break;

                case ProcessingState.TemporaryFailure:
                    donation.MarkAsTemporaryFailure();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(terminalState),
                        terminalState,
                        null);
            }

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            Assert.Equal(terminalState, donation.ProcessingState);

            _paymentProvider.Verify(
                provider =>
                    provider.ProcessPaymentAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<decimal>(),
                        It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_WhenPaymentSucceeds_SavesChangesTwice()
        {
            // Arrange
            var donation = new Donation(Guid.NewGuid(), "John Doe", 25m);

            _donationRepository
                .Setup(repository =>
                    repository.FirstOrDefaultAsync(
                        It.IsAny<System.Linq.Expressions.Expression<Func<Donation, bool>>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(donation);

            _paymentProvider
                .Setup(provider =>
                    provider.ProcessPaymentAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<decimal>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new PaymentResult(
                        PaymentResultStatus.Success));

            // Act
            await _service.ProcessAsync(donation.Id, CancellationToken.None);

            // Assert
            _unitOfWork.Verify(unitOfWork =>
                unitOfWork.SaveChangesAsync(
                    It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
    }
}
