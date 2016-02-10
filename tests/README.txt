The *tests* folder contains 2 subfolders:
=============================================

- ClippedArea: here is a clipped portion of the Big Sur study area
	       The species files, climate, and all required LANDIS files
	       contain definition for the full set of species/cohorts and ecoregions
	       identified for the entire study area. HOWEVER, LANDIS-II only reads the 
	       species/cohorts and ecoregions that are defined in the .img files of the
	       clipped area. The clipped area is 212 rows * 207 columns with
	       cell size = 30 m x 30 m. Projected coordinate system is NAD_1983_Albers.


- FakeArea:    here is a completely artificial area, created, as described in the
	       .doc file, for the purpose of testing alternative combinations
	       of initial communities and ecoregions, as well as initial sites of infection.
               These files were created to make sure the extension does not work improperly
	       or have unexpected behaviors. For more details, please read the .doc file
               inside this folder. The fake area is 100 rows * 100 columns
               with cell size = 100 m x 100 m. No coordinate system defined/needed.