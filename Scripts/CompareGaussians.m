% ========================
% Testing with random number
% ========================
mu = [2 10];
var_y = [25 20 15 10 8 6 4 3];
for i=1:8
    SIGMA = [1 1.5; 1.5 var_y(i)];
    % SIGMA = [1 1.5; 1.5 3];
    r = mvnrnd(mu,SIGMA,1000);
    figure;
%     plot(r(:,1),r(:,2),'.');
    hold on
    options = statset('Display','final');
    obj = gmdistribution.fit(r,1,'Options',options);
    ezsurf(@(x,y)pdf(obj,[x y]),[-2 6],[4 16])
    % ezsurf(@(x,y)pdf(obj,[x y]),[-20 20],[-20 20])
%     hold off
    AXIS([-2 6 4 16 0 0.2]);
    [V,D] = eig(SIGMA);
%     figure;
    o = [2 10];   %# Origin. Same as mu
    a = V(:,1)';  %# Eigenvector 1
    b = V(:,2)';  %# Eigenvector 2
    arrowStarts = [o; o];        %# Starting points for arrows
    arrowEnds = [a*D(1,1)+o; b*D(2,2)+o];          %# Ending points for arrows
    arrow(arrowStarts,arrowEnds);   %# Plot arrows
end;

% mu1 = [1 2];
% sigma1 = [3 .2; .2 2];
% mu2 = [-1 -2];
% sigma2 = [2 0; 0 1];
% X = [mvnrnd(mu1,sigma1,200);mvnrnd(mu2,sigma2,100)];
% 
% scatter(X(:,1),X(:,2),10,'ko')