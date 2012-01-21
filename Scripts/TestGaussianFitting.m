% ========================
% Try real dist map
% ========================
% Read in map file
map = csvread('C:\Lanny\MAMI\IPPA\Maps\DistMaps\5_modal_identical.csv');
[height, width] =  size(map);
% Convert bin results back to points
r = [];
for x = 1:height
    for y = 1:width
        r1 = [x 0;0 y];
        r2 = ones(uint8(map(x, y)),2)*r1;
        r = [r;r2];
    end;
end;

% obj = gmdistribution.fit(r,2);
% figure;
% ezsurf(@(x,y)pdf(obj,[x y]),[0 60],[0 60])
% axis([0 height 0 width 0 1]);

clc;
N = 5;
[modes, MUs, SigmaXSigmaY] = IPPAGaussianFitting(r, N);

