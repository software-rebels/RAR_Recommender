

#pvalues=c(dynamic.norm,dynamic.static,norm.static)
#comparison=c(c(rep("dynamic vs norm",length(dynamic.norm))),
#c(rep("dynamic vs static",length(dynamic.static))),
#c(rep("static vs norm",length(norm.static))))

#m <-  as.data.frame(cbind(pvalues,comparison))
#print(m)
#df$pvalue=as.numeric(df$pvalue)
base_breaks <- function(n = 3){
  function(x) {
    axisTicks(range(x, na.rm = TRUE), log = FALSE, n = n)
  }
}

df$pvalue <- as.double(df$pvalue)

ggplot(df, aes(type,pvalue),shape=factor(project)) +
  facet_grid(vars(threshold), scales="free")+
  theme_bw()+
  geom_point(aes(shape = project),size=5,alpha=0.5)+
  #scale_y_continuous(trans = 'log2',labels = scales::scientific)+
  scale_y_continuous(trans=scales::pseudo_log_trans(base = 10),
                     labels = scales::number_format(accuracy = 0.01,decimal.mark = '.'),
                     breaks = base_breaks(),
                     limits = c(0, 0.2))+
  geom_hline(yintercept =0.05 ,alpha=0.5, color="red")+
  labs( x = "Method Pairs", y = "P-Values",shape = "Projects: \n")+
  theme(
    panel.background = element_rect(fill = NA),
    axis.text = element_text(size=20, hjust=1,color = "black"),
    axis.title=element_text(size=24),
    strip.text.x = element_text(size = 18),
    strip.text.y = element_text(size = 18),
    legend.text = element_text(size = 18),
    legend.position = "top",
    legend.title = element_text(size = 18),
    axis.text.x=element_text(size=18, hjust=0.5,vjust=0.2)
  )
