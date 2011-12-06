% ========================
% Try real dist map
% ========================
% Test data:
% map = [1 2 3 4 5; 6 7 8 9 10; 11 12 13 14 15]
% height = 3;
% width = 5;
% Real data:
% Read in map file
map = csvread('C:\Lanny\MAMI\IPPA\Maps\DistMaps\Unimodal_Real.csv');
[height, width] =   size(map);
% Convert bin results back to points
r = [];
for x = 1:height
    for y = 1:width
        r1 = [x 0;0 y];
        r2 = ones(uint8(map(x, y)),2)*r1;
        r = [r;r2];
    end;
end;
obj = gmdistribution.fit(r,1);
ezsurf(@(x,y)pdf(obj,[x y]),[0 50],[0 50])



% ========================
% Testing with random number
% ========================
% mu = [2 10];
% SIGMA = [1 1.5; 1.5 3];
% r = mvnrnd(mu,SIGMA,1000);
% plot(r(:,1),r(:,2),'.');
% hold on
% options = statset('Display','final');
% obj = gmdistribution.fit(r,1,'Options',options);
% ezsurf(@(x,y)pdf(obj,[x y]),[-2 6],[4 16])


% ========================
% Testing with mixed Gaussians
% ========================
% 
% MU1 = [1 2];
% SIGMA1 = [2 0; 0 .5];
% MU2 = [-3 -5];
% SIGMA2 = [1 0; 0 1];
% X = [mvnrnd(MU1,SIGMA1,1000);mvnrnd(MU2,SIGMA2,1000)];
% 
% scatter(X(:,1),X(:,2),10,'.')
% hold on 
% 
% options = statset('Display','final');
% obj = gmdistribution.fit(X,1,'Options',options);
% 
% 
% % 10 iterations, log-likelihood = -7046.78
% 
% h = ezcontour(@(x,y)pdf(obj,[x y]),[-8 6],[-8 6]);

