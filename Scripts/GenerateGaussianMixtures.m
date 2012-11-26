% mu = [2 10];
% SIGMA = [1 1.5; 1.5 25];
% 
% r = mvnrnd(mu,SIGMA,1000);
% figure;
% %     plot(r(:,1),r(:,2),'.');
% hold on
% options = statset('Display','final');
% obj = gmdistribution.fit(r,1,'Options',options);
% ezsurf(@(x,y)pdf(obj,[x y]),[-2 6],[4 16])
% % ezsurf(@(x,y)pdf(obj,[x y]),[-20 20],[-20 20])
% %     hold off
% AXIS([-2 6 4 16 0 0.2]);
% [V,D] = eig(SIGMA);
% %     figure;
% o = [2 10];   %# Origin. Same as mu
% a = V(:,1)';  %# Eigenvector 1
% b = V(:,2)';  %# Eigenvector 2
% arrowStarts = [o; o];        %# Starting points for arrows
% arrowEnds = [a*D(1,1)+o; b*D(2,2)+o];          %# Ending points for arrows
% arrow(arrowStarts,arrowEnds);   %# Plot arrows

x1 = 0:1:100; x2 = 0:1:100;
[X1,X2] = meshgrid(x1,x2);

% #3
mu1 = [76.430641626628 71.7990966517948];
Sigma1 = [66.0425657206929  26.3403804072988; 26.3403804072988 57.01660344538];
F1 = mvnpdf([X1(:) X2(:)],mu1,Sigma1);
F1 = reshape(F1,length(x2),length(x1));
P1 = 0.152027980643134;

% #4
mu2 = [73.8605162373964 92.05108449106];
Sigma2 = [12.84453587809 6.45216686591825; 6.45216686591825 11.1027211891104];
F2 = mvnpdf([X1(:) X2(:)],mu2,Sigma2);
F2 = reshape(F2,length(x2),length(x1));
P2 = 0.0279985786416308;

% #1
mu3 = [56.9722043140986 49.5203986471138];
Sigma3 = [121.6966881243 -9.87336042956631; -9.87336042956631 191.353006437428];
F3 = mvnpdf([X1(:) X2(:)],mu3,Sigma3);
F3 = reshape(F3,length(x2),length(x1));
P3 = 0.394971094085163;

% #5
mu4 = [34.5022074953599 76.6094259846017];
Sigma4 = [338.649365791908  -106.999514634154; -106.999514634154 179.596200236722];
F4 = mvnpdf([X1(:) X2(:)],mu4,Sigma4);
F4 = reshape(F4,length(x2),length(x1));
P4 = 0.280956918282523;


% #2
mu5 = [59.2039802097763 18.2142834011275];
Sigma5 = [126.709352214597 -54.7628523139743; -54.7628523139743 45.6770992845049];
F5 = mvnpdf([X1(:) X2(:)],mu5,Sigma5);
F5 = reshape(F5,length(x2),length(x1));
P5 = 0.144045428347547;


% Gaussian number 1: 1, 
% Gaussian number 2: 0.869686967544242, 
% Gaussian number 3: 1.38703376065978, 
% Gaussian number 4: 0.460966193235638, 
% Gaussian number 5: 1.01059601806991

Final = F1*P1 + F2*P2 + F3*P3 + F4*P4 + F5*P5;

surf(x1,x2,Final);
caxis([min(Final(:))-.5*range(Final(:)),max(Final(:))]);
axis([0 100 0 100])
% xlabel('x1'); ylabel('x2'); zlabel('');