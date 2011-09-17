    % Setting environment variable
format compact;

% Read in map file
map = csvread('C:\Lanny\CS678\TrainingMaps_Before\pf.csv');
% Read in path file
pathfound = csvread('C:\Lanny\CS678\Results\pf_PF_path.csv');
% Read in parameter file
param = csvread('C:\Lanny\CS678\Results\pf_PF_param.csv');

% Specify specific parameters
[height, width] = size(map);
flytime = param(1);
% s=[param(2)+1;param(3)+1];
s=[param(3)+1;param(2)+1];
fprintf('Starting position: (%d,%d)\n', s(1), s(2));
upperbound = param(4);

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

figure(2);
clf;
% hAx5=subplot('position', [0.8 0.77 0.18 0.18]); % Convolution plot
% hAx4=subplot('position', [0.8 0.53 0.18 0.18]); % Percent of best possible plot
% hAx3=subplot('position', [0.8 0.28 0.18 0.18]); % Percent of total plot
% hAx2=subplot('position', [0.8 0.03 0.18 0.18]); % Total probablilty collected
% hAx1=subplot('position', [0.03 0.03 0.7 0.94]); % 3D Surface of probability distribution
hAx1=subplot('position', [0 0 1 1]); % 3D Surface of probability distribution
% Draw the surface
hSurf1=surf(hAx1,X,Y,img2);
colormap jet;
axis([0 height 0 width 0 255]);
drawnow;
% Maximize(2);

% Total cummulative probaiblity on the map
TotalValue = sum(img2(:));
% What is the max probability possible (If the UAV can teleport)
MaxProbability=upperbound;

hold(hAx1, 'on');
imgTemp = img2;

axes(hAx1);
set(plot3(s(1),s(2),imgTemp(s(1),s(2)),'k.'),'markersize',25, 'MarkerEdgeColor', 'y');
set(plot3(s(1),s(2),300,'k.'),'markersize',15, 'MarkerEdgeColor', 'y');
drawnow;

path=zeros(flytime+2, 4);
repeat=zeros(height, width);
path(1,1:2)=[0,0];
path(1,3)=0;
path(1,4)=0;  

% Doing starting node seperately so it will be marked as yellow dot.
path(2,1:2)=[s(1),s(2)];
% path(2,1:2)=[s(2),s(1)];
repeat(s(1),s(2))=repeat(s(1),s(2))+1;
% repeat(s(2),s(1))=repeat(s(2),s(1))+1;
path(2,3)=imgTemp(s(1),s(2));
% path(2,3)=imgTemp(s(2),s(1));
path(2,4)=path(1,4)+path(2,3);
imgTemp(s(1),s(2))=0;

pause;

for (i=1:flytime)
    % Cumulating probabilities
%     s(1) = pathfound(i+1,1)+1;
%     s(2) = pathfound(i+1,2)+1;
    s(2) = pathfound(i+1,1)+1;
    s(1) = pathfound(i+1,2)+1;
    
    path(i+2,1)=s(1);    
    path(i+2,2)=s(2);
    repeat(s(1),s(2))=repeat(s(1),s(2))+1;
    path(i+2,3)=imgTemp(s(1),s(2));
    path(i+2,4)=path(i+1,4)+path(i+2,3);
    pTemp=path(i+2,4);

    % Plot flying path
%     axes(hAx1);
    set(plot3(s(1),s(2),imgTemp(s(1),s(2)),'k.'),'markersize',15);
    set(plot3(s(1),s(2),300,'k.'),'markersize',15, 'MarkerEdgeColor', 'r');

    % The next line changes probability along flying path all to 0;
    imgTemp(s(1),s(2))=0;
    
%     % Drawing CDF
%     axes(hAx2);
%     plot(path(1:i+2,4));
%     title(['Step=', num2str(i), ' Cumulative p=', num2str(pTemp)]);
%     axis tight;
%     axes(hAx3);
%     plot(path(1:i+2,4)/TotalValue);
%     title(['Total=', num2str(TotalValue), ' Percent= ', num2str(pTemp/TotalValue*100, '%6.3f'), '%']);
%     axis tight;
%     axes(hAx4);
%     plot(path(1:i+2,4)/MaxProbability);
%     title(['Max=', num2str(MaxProbability), ' Percent= ', num2str(pTemp/MaxProbability*100, '%6.3f'), '%']);        
%     axis tight;        

    % Drawing the probability change as we go;
    set(hSurf1, 'ZData', imgTemp);

    % Drawing as animation instead of final result;
    drawnow;
end;

% axes(hAx1);
set(plot3(s(1),s(2),imgTemp(s(1),s(2)),'k.'),'markersize',25, 'MarkerEdgeColor', 'b');
set(plot3(s(1),s(2),300,'k.'),'markersize',15, 'MarkerEdgeColor', 'b');

% hold(hAx1, 'off');
drawnow;

%     figure(4);
%     clf;
%     surf(X,Y,imgTemp);
%     colormap jet;
%     drawnow;
%     Maximize(4);

%     figure(3);
%     axes(hAx1);
%     axis vis3d;
%     for i=1:3600
%         camorbit(1,0,'data', [0,0,1]);
%         drawnow;
%     end;

% What is the best solution found so far?    
fprintf('%d cells were visited 2 times, and %d cells were visited more than 2 times.\n', length(find(repeat==2)), length(find(repeat>2)));
