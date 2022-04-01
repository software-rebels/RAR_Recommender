# Replication Package
This repository contains the required dataset/script to replicate the results for the study "Exploring the Notion of Risk in Reviewer Recommendation." This code extends the Relationalgit package (https://github.com/CESEL/RelationalGit) and adds the functionalities which incorporate the concept of fix-inducing likelihood of a project.

# Dependencies
To run the code and replicate the results, you should first install the following tools:

## 1) .NET Core
 [.NET Core](https://www.microsoft.com/net/download).

 [Microsoft Visual Studio](https://visualstudio.microsoft.com/).
## 2) SQL Server
[SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) to import the databases.

SQL Server - [LocalDb, Express, and Developer Editions](https://www.microsoft.com/en-ca/sql-server/sql-server-downloads).

# Import data:
After installing the necessary tools, download the dataset files from [here](https://zenodo.org/record/6407313#.YkeKcujMI2o) and import different projects using SQL Server Management Studio. 
After importing the files, you have to update the database credentials in the files that need a connection to the database. These files are located inside:
- "/notebooks"
- "/ReplicationPackage"

## Step 1: Prepare the model and data (data exist in the replication package)
### 1.1 Metric extraction from Github
Start by running "/notebooks/RQ1_PRMetricExtraction.ipynb" to extract metrics from Github; Fill the Github id and token with your token and id.
### 1.2 Create defect prediction model 
Run the "/notebooks/RQ1_CreatePeriodicModels.ipynb" notebook and create the defect prediction models for the studied projects. 
### 1.3 Analyze the distribution of predicted defect proneness
Run the RQ1 notebook for analyziz "/notebooks/RQ1_Figure-balanced_accuracy.ipynb" to see the distribution of the predicted defect proneness.


## Step 2: (RQ1) How do existing CRR approaches perform with respect to the risk of inducing future fixes?
### 2.1 Running C# project
Run the C# code as the simulator for different projects. You can open Microsoft Visual Studio and open the project properties. Under the debug menu, click on "RelationalGit Debug Protperties". It will take few minutes to load first time. Once loaded, in the "Application Argument" textbox, run the commands that replicate the results.
### 2.2 Running simulations for existing CRR approaches
For RQ 1, copy the following command and click the run button (green play button with "Relational Git" next to it, or press F5) :
```
--cmd simulate-recommender --recommendation-strategy Strategy --conf-path  "Absolute/address/to/json/setting.json"
```
Where strategy should be one of: 
- Reality
- LearnRec
- RetentionRec
- TurnoverRec
- cHRev
- AuthorshipRec
- RevOwnRec
The first startegy calculates the evaluation metrics for real reviewers of the project, as the baseline for each comparison.
The placeholder "Absolute/address/to/json/setting.json should be replaced with the json files in the "/ReplicationPackage" folder as the setting of the simulation.
### 2.3 Analyzing the simulation results
Once the replication is finished, run the following command to compare the CRR approach with reality:
```
--cmd analyze-simulations --analyze-result-path "absolute/path/to/save/results" --recommender-simulation recommendation_id  --reality-simulation reality_simulation_id --conf-path  "Absolute/address/to/json/setting.json"
```
The *recommendation_id* and *reality_simulation_id* are ids for the CRR approach under analysis and the reality run in the database.
The results for the RQ1 analysis are available in the '/notebooks/RQ1' folder.


## Step 3: (RQ2) How can the risk of fix-inducing code changes be effectively balanced with other quantities of interest?
### 3.1 Running RAR CRR for various values of P<sub>D</sub>
To replicate the results for the RQ2 analysis, run the simulation using the 'RAR CRR.' Change the value of P<sub>D</sub> in line 69 of "src\RelationalGit.Recommendation\Strategies\Spreading\JITSofiaRecommendationStrategy.cs" file. We changed the value between 0.1 and 0.9 with 0.1 intervals in this our study. 
### 3.2 Analyzing the results of simulations
After running all the experiments, compare the results against reality. The results should be organized in a format similar to "/notebooks/RQ2/". To see the results, run the RQ2 notebooks.

## Step 4: (RQ3) How can we identify an effective fix-inducing likelihood threshold (PD ) interval for a given project?
### 4.1: Initialize the methods and calculate boundaries
To replicate these experiments,run the "/notebooks/RQ3_calculate_boundaries.ipynb" notebook. This notebook finds the dynamic and normalized method boundaries. Once it is done, comment line 69 and uncomment line 70 in "src/RelationalGit.Recommendation/Strategies/Spreading/JITSofiaRecommendationStrategy.cs" file. 
### 4.2 Running the simulations
After running the experiment for three Quartiles (Q<sub>1</sub>, Q<sub>2</sub>, and Q<sub>3</sub>) for each method, analyze the result and put them in the format of "/notebooks/RQ3/" folder. 
### 4.3 Analyzing the results
Next, run the "/notebooks/RQ3_merge_csvs.ipynb" notebook to merge these data into one CSV. Move the resultant CSV into the folder containing the R script, replacing the previous version of it.

Then you can run the R script in the following order:
- '/R_scripts_RQ3/friedman_test.R': Load files and run Friendman test.
- '/R_scripts_RQ3/friedman_draw_plot.R': Draw the results of the Friedman test.
- '/R_scripts_RQ3/Conover_with_holm-bonferrani_method.R': Run Conover test.
- '/R_scripts_RQ3/conover_draw_plot.R': Draw the results of the Conover test.
