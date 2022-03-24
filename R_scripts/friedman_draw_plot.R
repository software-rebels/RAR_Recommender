# draw plot
data=read.csv("./defect_expertiseloss.csv")
ggplot(data, aes(res, method)) +
  geom_violin(aes(fill=res),draw_quantiles = c(0.25, 0.5, 0.75),fill='#e0e0e0') +
  facet_grid(vars(threshold), vars(project), scales="free")+
  theme_bw()+
  geom_point(position = position_jitter(seed = 1, width = 0.1),size=0.5,alpha = 0.2 )+
  theme(
        panel.background = element_rect(fill = NA),
        axis.text = element_text(size=20, hjust=1,color = "black"),
        axis.title=element_text(size=24),
        strip.text.x = element_text(size = 18),
        strip.text.y = element_text(size = 18),
        
        )+
  labs( x = "Performance Improvement (%)", y = "Methods")

