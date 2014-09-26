pdflatex -halt-on-error -draftmode -interaction nonstopmode report.tex
bibtex main
pdflatex --halt-on-error -draftmode -interaction nonstopmode report.tex
pdflatex --halt-on-error -interaction nonstopmode report.tex
