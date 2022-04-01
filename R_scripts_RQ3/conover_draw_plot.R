

Method_name <- list(
  '0.25'="Risk-averse",
  '0.5'="Risk-balanced",
  '0.75'="Risk-tolerant ",
)

method_labeller <- function(variable,value){
  return(Method_name[value])
}


base_breaks <- function(n = 3){
  function(x) {
    axisTicks(range(x, na.rm = TRUE), log = FALSE, n = n)
  }
}

df$pvalue <- as.double(df$pvalue)

ggplot(df, aes(type,pvalue),shape=factor(project)) +
  facet_grid(vars(threshold), scales="free",labeller=hospital_labeller)+
  theme_bw()+
  geom_point(aes(shape = project),size=2.3,alpha=0.5)+
  scale_y_continuous(trans=scales::pseudo_log_trans(base = 10),
                     labels = scales::number_format(accuracy = 0.01,decimal.mark = '.'),
                     breaks = base_breaks(),
                     limits = c(0, 0.2))+
  geom_hline(yintercept =0.05 ,alpha=0.5, color="red")+
  labs( x = "Method Pairs", y = "P-Values",shape = "Projects: \n")+
  theme(
    panel.background = element_rect(fill = NA),
    axis.text = element_text(size=11, hjust=1,color = "black"),
    axis.title=element_text(size=14),
    strip.text.x = element_text(size = 10),
    strip.text.y = element_text(size = 10),
    legend.text = element_text(size = 13),
    legend.position = "top",
    legend.title = element_text(size = 13),
    axis.text.x=element_text(size=12, hjust=0.5,vjust=0.2),
    legend.margin=margin(0,0,0,0),
    legend.box.margin=margin(-0,-20,-10,-20)
  )
