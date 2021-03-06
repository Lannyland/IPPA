% Setting environment variable
format compact;

% Read in map file
% map = csvread('C:\Lanny\MAMI\IPPA\Maps\DistMaps\Real_BimodalFar.csv');
% map = csvread('C:\Lanny\MAMI\IPPA\Maps\DiffMaps\Diff_Real_BimodalFar.csv');
% map = csvread('C:\Lanny\MAMI\IPPA\Maps\DistMaps\Real_BimodalClose.csv');
% map = csvread('C:\Lanny\MAMI\IPPA\Maps\DiffMaps\Diff_Real_BimodalClose.csv');
map = csvread('C:\Lanny\MAMI\IPPA\Maps\DistMaps\Real_Complex2.csv');
% map = csvread('C:\Lanny\MAMI\IPPA\Maps\DiffMaps\Diff_Real_Complex2.csv');

% Specify specific parameters
[height, width] =   size(map);

% Scale so min is 0 and max is 255
% 1. Find min and max
% min = 
        
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
%%% Create a grid of states     %%%
%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
img2 = double(map);
X = zeros(width, height);
Y = zeros(width, height);
for x = 1:height
    for y = 1:width
        Y(y,x) = x;
        X(y,x) = y;
    end;
end;

figure;
clf;
hAx1=subplot('position', [0 0 1 1]); % 3D Surface of probability distribution
% Draw the surface
hSurf1=surf(hAx1,X.',Y.',fliplr(img2));
colormap jet;
axis([0 height 0 width 0 255]);
% axis([0 height 0 width 0 4]);
drawnow;
