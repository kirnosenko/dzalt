docker build -f "../src/DZALT.Web/Dockerfile" --force-rm -t dzalt ".."
docker tag dzalt kirnosenko/dzalt
