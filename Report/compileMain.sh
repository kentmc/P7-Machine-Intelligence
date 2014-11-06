REPORT_MAIN=report
pdflatex -halt-on-error -draftmode -interaction nonstopmode $REPORT_MAIN.tex 
pdflatex --halt-on-error -draftmode -interaction nonstopmode $REPORT_MAIN.tex 
bibtex $REPORT_MAIN 
makeglossaries $REPORT_MAIN.glo
pdflatex --halt-on-error -interaction nonstopmode $REPORT_MAIN.tex 
./cleanUp.sh
