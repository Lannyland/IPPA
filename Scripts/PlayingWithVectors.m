o = [0 0];  %# Origin
a = [2 1];  %# Vector 1
b = [2 2];  %# Vector 2
c = a+b;      %# Resultant
arrowStarts = [o; a; o];        %# Starting points for arrows
arrowEnds = [a; c; c];          %# Ending points for arrows
arrow(arrowStarts,arrowEnds);   %# Plot arrows