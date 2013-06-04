% Setting environment variable
format compact;
findex = 35;

% Parameters
isCopter = 1;               % 1 yes 0 no (default to copter)
curPos = [51 51];             % UAV Position
% curPos = [1 1];             % UAV Position
% curPos = [31 31];             % UAV Position
parent = [-1, -1];          % Previous position used to not fly backward

% Read in distribution map (Should be smoothed with low pass filter -
% convolution.
% fileName = '../Maps/DistMaps/TestDistMap.csv';
% fileName = '../Maps/DistMaps/Smoothed_Small_HikerPaulDist.csv';
fileName = '../Maps/DistMaps/Smoothed_Small_NewYork53Dist.csv';
% fileName = '../Maps/DistMaps/Smoothed_Small_NewYork108Dist.csv';
distMap = csvread(fileName);
[height,width,depth] = size(distMap);
% Normalize distribution map
distMap = distMap./sum(sum(distMap));

% Read in difficulty map
% fileName = '../Maps/DiffMaps/TestDiffMap.csv';
% fileName = '../Maps/DiffMaps/Small_HikerPaulDiff.csv';
fileName = '../Maps/DiffMaps/Small_NewYork53Diff.csv';
% fileName = '../Maps/DiffMaps/Small_NewYork108Diff.csv';
diffMap = csvread(fileName);

% Build real difficulty map (in percentage)
% Identify max difficulty
maxDiff = max(max(diffMap));
realDiffMap = diffMap /(maxDiff+1);
diffMap = realDiffMap;

% If no diffMap is used
diffMap = zeros(height, width);

% Read in path file
UAVPath = csvread('C:\Lanny\MAMI\IPPA\Maps\Paths\NewYork53_900_NoDiff_TopNH_Path.txt');
% UAVPath = csvread('C:\Lanny\MAMI\IPPA\Maps\Paths\NewYork53_900_YesDiff_TopNH_Path.txt');
[T,junk] = size(UAVPath);
% Fix path to 1 based instead of 0 based
UAVPath = UAVPath + 1;
% Horizentally flip path matrix from (x, y) to (row, column) 
UAVPath = fliplr(UAVPath);

% Draw prior PDF
figure(findex);
clf;
X = zeros(width, height);
Y = zeros(width, height);
for x = 1:height
    for y = 1:width
        Y(y,x) = y;
        X(y,x) = x;
    end;
end;
% 3D Surface of probability distribution
hAx1=subplot('position', [0 0 1 1]); 
% Draw the surface
% hSurf1=surf(hAx1,X,Y,imgin,'FaceColor','interp','FaceLighting','phong');
hSurf1=surf(hAx1,X,Y,distMap','EdgeColor','none','FaceColor','interp','FaceLighting','phong');
colormap jet;
% axis([0 height 0 width 0 1000]);
drawnow;
hold(hAx1, 'on');
% axes(hAx1);

pause;

% Loop through flight one time step at a time
for t = 1:T    
    % What's current position?
    curPos = UAVPath(t,:);
    % Compute detection probability (just for UAV location)
    ndp = diffMap(curPos(1,1), curPos(1,2));
    % Update
    distMap(curPos(1,1), curPos(1,2)) = distMap(curPos(1,1), curPos(1,2)) * ndp;
    % Drawing the probability change as we go;
    set(hSurf1, 'ZData', distMap', 'FaceColor', 'interp', 'FaceLighting','phong');
    % Plot flying path
%    set(plot3(curPos(1,2), curPos(1,1),distMap(curPos(1,1), curPos(1,2)),'r.'),'markersize',15);
    set(plot3(curPos(1,1), curPos(1,2),max(max(distMap)),'k.'),'markersize',15, 'MarkerEdgeColor', 'k');
    
    % Drawing as animation instead of final result;
    drawnow;
end;

