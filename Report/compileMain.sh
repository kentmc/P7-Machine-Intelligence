pdflatex -halt-on-error -draftmode -interaction nonstopmode report.tex
pdflatex --halt-on-error -draftmode -interaction nonstopmode report.tex
bibtex report 
makeglossaries report.glo
pdflatex --halt-on-error -draftmode -interaction nonstopmode report.tex
pdflatex --halt-on-error -interaction nonstopmode report.tex
./cleanUp.sh
