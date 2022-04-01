library(magrittr) # needs to be run every time you start R and want to use %>%
library(dplyr)
library(tidyverse)
library(ggpubr)
library(rstatix)
library(stats)
library(DescTools)
library(PMCMRplus)
setwd("/current/folder")

data=read.csv("./defect_expertiseloss.csv")
prjs <- c('Roslyn','Rust','Kubernetes')
thrs <- c('0.25','0.5','0.75')
dynamic.norm <- c()
dynamic.static <- c()
norm.static <- c()

df <- data.frame(threshold=factor(levels = unique(data$threshold)),
             project=factor( levels = unique(data$project)),
             pvalue=double(),
             type=factor(levels=c("dynamic\nvs norm","dynamic\nvs static","norm\nvs static")),
             stringsAsFactors=FALSE)
for (prj in prjs) {
  for (thr in thrs) {
    print('------------------')
    print(prj)
    print(thr)
    subData<- subset(data, (project==prj & threshold==thr))
    subData$method<-factor(subData$method)
    subData$PeriodId<-factor(subData$PeriodId)
    res.conover<-frdAllPairsConoverTest(subData$res,subData$method,subData$PeriodId,p.adjust.method = "holm")
    pvalues=res.conover$p.value
    
    df[nrow(df) + 1,] = c(thr,prj,pvalues[1,1],"dynamic\nvs norm")
    df[nrow(df) + 1,] = c(thr,prj,pvalues[2,1],"dynamic\nvs static")
    df[nrow(df) + 1,] = c(thr,prj,pvalues[2,2],"norm\nvs static")
    
    dynamic.norm <- c(dynamic.norm, pvalues[1,1])
    dynamic.static <- c(dynamic.static, pvalues[2,1])
    norm.static <- c(norm.static, pvalues[2,2])
    
    print(res.conover$p.value)
    
    
  }
}


