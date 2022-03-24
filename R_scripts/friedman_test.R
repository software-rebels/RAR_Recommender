library(magrittr) # needs to be run every time you start R and want to use %>%
library(dplyr)
library(tidyverse)
library(ggpubr)
library(rstatix)
library(stats)

setwd("/projectfolder")
data=read.csv("./defect_expertiseloss.csv") # This file is the output of the notebook file
prjs <- c('Roslyn','Rust','Kubernetes')
thrs <- c('0.25','0.5','0.75')
for (prj in prjs) {
  for (thr in thrs) {
    print('------------------')
    print(prj)
    print(thr)
    subData<- subset(data, (project==prj & threshold==thr))
    subData$method<-factor(subData$method)
    subData$PeriodId<-factor(subData$PeriodId)
    #res.aov <- subData %>% friedman.test(y=subData$res , group=subData$method, block=subData$PeriodId , data = subData)
    res.friedman_test <- subData %>% friedman_test(res ~ method | PeriodId)
    #res.friedman_test <- friedman.test(subData$res, subData$method, subData$PeriodId)
    print(res.friedman_test)
    res.KW<- subData %>%friedman_effsize(res ~ method | PeriodId)
    print(res.KW)
    
  }
}


