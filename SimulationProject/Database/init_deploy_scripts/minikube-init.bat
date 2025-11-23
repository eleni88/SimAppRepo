@echo off

echo Starting minikube...
minikube start --driver=docker

echo Installing Gateway API CRDs...
kubectl kustomize "https://github.com/nginx/nginx-gateway-fabric/config/crd/gateway-api/standard?ref=v2.0.2" | kubectl apply -f -

echo Deploying NGINX Gateway Fabric CRDs...
kubectl apply --server-side -f https://raw.githubusercontent.com/nginx/nginx-gateway-fabric/v2.0.2/deploy/crds.yaml

echo Deploying NGINX Gateway Fabric Controller...
kubectl apply -f https://raw.githubusercontent.com/nginx/nginx-gateway-fabric/v2.0.2/deploy/default/deploy.yaml

echo Cluster and Gateway API setup completed.
pause