function [modes, MUs, SigmaXSigmaY] = IPPAGaussianFitting(samples, N)

% This function is called from the IPPA app.
% Input Parameters:
% ==================
% samples:  Matrix representing samples points (probability map * difficulty map)
% N:        Number of Gaussians to fit
% Output Parameters:
% ==================
% modes:        Likelihood at modes for non-scaled Gaussians
% MUs:          List of mus in the order of Gaussians found (used for indexing)
% SigmaXSigmaY: Product of SigmaX and SigmaY for each eigen-aligned Gaussian

% Initialize output parameters
modes = zeros(N, 1);
MUs = zeros(N, 2);
SigmaXSigmaY = zeros(N, 1);

% Perform mixed Gaussian fitting
options = statset('MaxIter', 200);
obj = gmdistribution.fit(samples,N,'Options',options);
obj.Iters
figure;
ezsurf(@(x,y)pdf(obj,[x y]),[0 60],[0 60])

% Prepare output parameters
for i=1:N
    [V,D] = eig(obj.Sigma(:,:,i));
    MUs(i,:) = obj.mu(i,:);
    SigmaXSigmaY(i) = sqrt(D(1,1))*sqrt(D(2,2));
    modes(i) = mvnpdf(obj.mu(i,:),obj.mu(i,:),obj.Sigma(:,:,i));
end;