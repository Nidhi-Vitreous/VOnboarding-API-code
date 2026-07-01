using Vitreous.Onboarding.Domain.Enums;



namespace Vitreous.Onboarding.Api.Contracts;



public sealed class UpdateOnboardingStatusRequest

{

    public required OnboardingRequestStatus Status { get; init; }



    public OnboardingRequestStatus? PreviousStatus { get; init; }

}

