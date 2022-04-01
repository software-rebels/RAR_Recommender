# Replication Package
This repository contains the necessary data for replicating the necessary information to replicate the study of "Exploring the Notion of Risk in Reviewer Recommendation". This code extends the Relationalgit package (https://github.com/CESEL/RelationalGit), and add some functionlities that is needed to incorporate the concept of fix-inducing likelihood of a project.

# Dependencies
In order to run the code and replicate the results, you should first install the following tools:

## 1) .NET Core
 [.NET Core](https://www.microsoft.com/net/download).

## 2) SQL Server
[Sql Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) to import the databases.

Sql Server - [LocalDb, Express, and Developer Editions](https://www.microsoft.com/en-ca/sql-server/sql-server-downloads) 

# Import data:
Once you install the necessary tools, you can download the dataset files from [here]() and import them using Sql Server Management Studio. 
After importing the files, you have to update the database credntials in the files that needs connection to the datase. These files are locates inside:
- /notebooks
- /ReplicationPackage

## Step 1: Prepare the model and data (data exist in the replication package)
You start by running 'notebooks\RQ1_PRMetricExtraction.ipynb' to extract metrics from github, please fill the github id and token with your own token and id. Then, run the '/notebooks\RQ1_CreatePeriodicModels.ipynb' notebook and create the models for all the periods of the studied project. Then, you can run the other RQ1 notebook (notebooks\RQ1_Figure-balanced_accuracy.ipynb) to see the distribution of the predicted defect proness.

## Step 2: (RQ1) How do existing CRR approaches perform with respect to the risk of inducing future fixes?
Now you should run the simulator for differnt projects. You can open the visual studio.net and open the project properties. Under debug, run the following command:
```
--cmd simulate-recommender --recommendation-strategy Strategy --conf-path  "Absolute/address/to/json/setting.json"
```
Where Strategy should be replaced by one of: 
- Reality
- LearnRec
- RetentionRec
- TurnoverRec
- cHRev
- AuthorshipRec
- RevOwnRec

And the json file is the one resides in /ReplicationPackage folder.
Once the replication is finished, you can runthe following command to compare the CRR approach with reality:
```
--cmd analyze-simulations --analyze-result-path "absolute/path/to/save/results" --recommender-simulation recommendation_id  --reality-simulation reality_simulation_id --conf-path  "Absolute/address/to/json/setting.json"
```
The recommendation_id and reality_simulation_id are ids for the CRR under analyze and the reality run in the database.
you can see the results of this RQ1 available in '/notebooks/RQ1' folder.
## Step 3: (RQ2) How can the risk of fix-inducing code changes be effectively balanced with other quantities of interest?
For the next research question, you can run the simulation using the strategy of 'RAR'. You can change the value of P<sub>D</sub> in line 69 of 'src\RelationalGit.Recommendation\Strategies\Spreading\JITSofiaRecommendationStrategy.cs' file. In this RQ we changed the value between 0.1 and 0.9 with 0.1 intervals. 
After running all the experiments, you can compare the results against reality and put the in a format similar to '/notebooks/RQ2/'. The for the analysis, please run the RQ2 notebooks and see the resutls.

## Step 4: (RQ3) How can we identify an effective fix-inducing likelihood threshold (PD ) interval for a given project?
In this research question, we wanted to see if we can suggest any method to help defining a rang depending on the risk levels of the stakeholders. In order to replicate this experiments, please first run '/notebooks/RQ3_calculate_boundaries.ipynb' notbook.This notebook, find the dynamic and normalized boundaries for different periods. Once it is done, you can comment line 69 and uncomment line 70 in 'src\RelationalGit.Recommendation\Strategies\Spreading\JITSofiaRecommendationStrategy.cs' file. 
After running the experiment for three Quartiles (Q<sub>1</sub>, Q<sub>2</sub>, and Q<sub>3</sub>) for each method, you can analyze the result and put them in the format of '/notebooks/RQ3/' folder. 
Next steps, would be to run '/notebooks/RQ3_merge_csvs.ipynb' notebook to merge these data into one csv. This csv should then be placed in in the folder next to the R scripts (a sample merged files is already there).
Then you can run the R script in the following order:
- '/R_scripts_RQ3/friedman_test.R': Load files and run Friendman test.
- '/R_scripts_RQ3/friedman_draw_plot.R': Draw the results of the Friedman test.
- '/R_scripts_RQ3/Conover_with_holm-bonferrani_method.R': Run Conover test.
- '/R_scripts_RQ3/conover_draw_plot.R': Draw the results of the conover test.

This concludes the experiments in this study!