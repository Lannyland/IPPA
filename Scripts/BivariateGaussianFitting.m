% % ========================
% % Try real dist map
% % ========================
% % Test data:
% % map = [1 2 3 4 5; 6 7 8 9 10; 11 12 13 14 15]
% % height = 3;
% % width = 5;
% % Real data:
% % Read in map file
% map = csvread('C:\Lanny\MAMI\IPPA\Maps\DistMaps\Real_BimodalClose.csv');
% [height, width] =   size(map);
% % Convert bin results back to points
% r = [];
% for x = 1:height
%     for y = 1:width
%         r1 = [x 0;0 y];
%         r2 = ones(uint8(map(x, y)),2)*r1;
%         r = [r;r2];
%     end;
% end;
% obj = gmdistribution.fit(r,2);
% figure;
% ezsurf(@(x,y)pdf(obj,[x y]),[0 60],[0 60])
% axis([0 height 0 width 0 1]);


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
% MU1 = [21 42];
% SIGMA1 = [100 0; 0 64];
% MU2 = [35 45];
% SIGMA2 = [25 0; 0 9];

MU1 = [15 30];
SIGMA1 = [64 0; 0 64];
MU2 = [45 30];
SIGMA2 = [64 0; 0 64];

X = [mvnrnd(MU1,SIGMA1,1000);mvnrnd(MU2,SIGMA2,1000)];
Xbig = vertcat(X,X,X,X,X,X,X,X,X,X,X,X,X,X,X);

obj = gmdistribution.fit(Xbig,2);
figure;
ezsurf(@(x,y)pdf(obj,[x y]),[0 60],[0 60])
axis([0 60 0 60]);

Sigma1 = obj.Sigma(:,:,1);
Sigma2 = obj.Sigma(:,:,2);

[V1,D1] = eig(Sigma1);
[V2,D2] = eig(Sigma2);

a1 = sqrt(D1(1,1))
b1 = sqrt(D1(2,2))
area1 = 3*a1*3*b1
a2 = sqrt(D2(1,1))
b2 = sqrt(D2(2,2))
area2 = 3*a2*3*b2

mode1 = mvnpdf(mu, mu, Sigma1);
mode2 = mvnpdf(mu, mu, Sigma2);

