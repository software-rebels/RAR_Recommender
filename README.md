# Replication Package
This repository contains the required information for replicating the information required to replicate the study of "Exploring the Notion of Risk in Reviewer Recommendation." This code extends the Relationalgit package (https://github.com/CESEL/RelationalGit) and adds some functionalities needed to incorporate the concept of fix-inducing likelihood of a project.

# Dependencies
To run the code and replicate the results, you should first install the following tools:

## 1) .NET Core
 [.NET Core](https://www.microsoft.com/net/download).

## 2) SQL Server
[SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) to import the databases.

SQL Server - [LocalDb, Express, and Developer Editions](https://www.microsoft.com/en-ca/sql-server/sql-server-downloads) 

# Import data:
Once you install the necessary tools, you can download the dataset files from [here](https://zenodo.org/record/6403760#.Ykc9zW7MJuU) and import them using SQL Server Management Studio. 
After importing the files, you have to update the database credentials in the files that need a connection to the database. These files are located inside:
- /notebooks
- /ReplicationPackage

## Step 1: Prepare the model and data (data exist in the replication package)
You start by running 'notebooks\RQ1_PRMetricExtraction.ipynb' to extract metrics from Github; please fill the Github id and token with your token and id. Then, run the '/notebooks\RQ1_CreatePeriodicModels.ipynb' notebook and create the models for all the periods of the studied project. Then, you can run the other RQ1 notebook (notebooks\RQ1_Figure-balanced_accuracy.ipynb) to see the distribution of the predicted defect proneness.

## Step 2: (RQ1) How do existing CRR approaches perform with respect to the risk of inducing future fixes?
Now you should run the simulator for different projects. You can open visual studio.net and open the project properties. Under debug menu, click on RelationalGit Debug Protperties". It will take a couple of minutes to load. Once loaded, in the "Application Argument" textbox, you can run the commands of this replications package.
For RQ 1, copy the following command and click the run button (green play button with "Relational Git" next to it, or press F5) :
```
--cmd simulate-recommender --recommendation-strategy Strategy --conf-path  "Absolute/address/to/json/setting.json"
```
Where strategy should be replaced by one of: 
- Reality
- LearnRec
- RetentionRec
- TurnoverRec
- cHRev
- AuthorshipRec
- RevOwnRec

And the JSON file is the one that resides in the /ReplicationPackage folder.
Once the replication is finished, you can run the following command to compare the CRR approach with reality:
```
--cmd analyze-simulations --analyze-result-path "absolute/path/to/save/results" --recommender-simulation recommendation_id  --reality-simulation reality_simulation_id --conf-path  "Absolute/address/to/json/setting.json"
```
The recommendation_id and reality_simulation_id are ids for the CRR under analysis and the Reality run in the database.
You can see the results of this RQ1 available in the '/notebooks/RQ1' folder.
## Step 3: (RQ2) How can the risk of fix-inducing code changes be effectively balanced with other quantities of interest?
For the next research question, you can run the simulation using the 'RAR CRR.' You can change the value of P<sub>D</sub> in line 69 of 'src\RelationalGit.Recommendation\Strategies\Spreading\JITSofiaRecommendationStrategy.cs' file. We changed the value between 0.1 and 0.9 with 0.1 intervals in this RQ. 
After running all the experiments, you can compare the results against reality and put them in a format similar to '/notebooks/RQ2/'. For the analysis, please run the RQ2 notebooks and see the results.

## Step 4: (RQ3) How can we identify an effective fix-inducing likelihood threshold (PD ) interval for a given project?
In this research question, we wanted to see if we could suggest any method to help define a range depending on the risk levels of the stakeholders. To replicate these experiments, please run the '/notebooks/RQ3_calculate_boundaries.ipynb' notebook. This notebook finds the dynamic and normalized boundaries for different periods. Once it is done, you can comment line 69 and uncomment line 70 in 'src\RelationalGit.Recommendation\Strategies\Spreading\JITSofiaRecommendationStrategy.cs' file. 
After running the experiment for three Quartiles (Q<sub>1</sub>, Q<sub>2</sub>, and Q<sub>3</sub>) for each method, you can analyze the result and put them in the format of '/notebooks/RQ3/' folder. 
The next step would be to run the '/notebooks/RQ3_merge_csvs.ipynb' notebook to merge these data into one CSV. The resulted CSV should then be placed in the folder next to the R scripts (a sample merged CSV file is already there).
Then you can run the R script in the following order:
- '/R_scripts_RQ3/friedman_test.R': Load files and run Friendman test.
- '/R_scripts_RQ3/friedman_draw_plot.R': Draw the results of the Friedman test.
- '/R_scripts_RQ3/Conover_with_holm-bonferrani_method.R': Run Conover test.
- '/R_scripts_RQ3/conover_draw_plot.R': Draw the results of the Conover test.

This concludes the experiments in this study!