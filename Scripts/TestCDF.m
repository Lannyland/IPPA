mu = [0 0];
sigma_x = 2;
sigma_y = 3;
SIGMA = [sigma_x*sigma_x 0;0 sigma_y*sigma_y];
% X = mvnrnd(mu, SIGMA, 10);
% p = mvnpdf(X, mu, SIGMA);
[X1,X2] = meshgrid(linspace(-(sigma_x*3),sigma_x*3,7)',linspace(-(sigma_y*3),sigma_y*3,7)');
X = [X1(:) X2(:)];
p = mvncdf(X,mu,SIGMA);
surf(X1,X2,reshape(p,7,7));
v_sigma1 =  p(7*2+3) - p(7*2+5) - p(7*4+3) + p(7*4+5)
v_sigma2 =  p(7*1+2) - p(7*1+6) - p(7*5+2) + p(7*5+6)
v_sigma3 =  p(7*0+1) - p(7*0+7) - p(7*6+1) + p(7*6+7)