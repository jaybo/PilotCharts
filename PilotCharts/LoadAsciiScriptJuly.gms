GLOBAL_MAPPER_SCRIPT VERSION=1.00

DEFINE_PROJ PROJ_NAME="GEO_WGS84"
Projection     GEOGRAPHIC
Datum          WGS84
Zunits         NO
Units          DD
Xshift         0.000000
Yshift         0.000000
Parameters
180 0 0.000 /* longitude of center of projection

END_DEFINE_PROJ
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.0.25.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.0.5.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.1.0.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.2.0.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.4.0.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.8.0.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 
IMPORT_ASCII FILENAME="E:\Deepzoom\PilotCharts\out.July.16.0.txt" TYPE=POINT_ONLY COORD_DELIM=COMMA  INC_ELEV_COORDS=NO COORD_ORDER=Y_FIRST  PROJ_NAME=GEO_WGS84 HIDDEN=YES 


