@echo off

set "BACKEND_ROOT=%~1"
set "FRONTEND_ROOT=%~2"

if "%BACKEND_ROOT%"=="" (
    echo [ERROR] backend root path not found.
    pause
    exit /b 1
)

if "%FRONTEND_ROOT%"=="" (
    echo [ERROR] frontend root path not found.
    pause
    exit /b 1
)

echo Backend root:  %BACKEND_ROOT%
echo Frontend root: %FRONTEND_ROOT%
echo.

echo Building DB image...
docker build -t simulation-db:latest "%BACKEND_ROOT%\Database"

echo Loading image to minikube...
minikube image load simulation-db:latest

echo Applying database manifests...
kubectl apply -f "%BACKEND_ROOT%\Database\db-service.yaml"
kubectl apply -f "%BACKEND_ROOT%\Database\db-statefulset.yaml"

echo Applying backend manifests...
kubectl apply -f "%BACKEND_ROOT%\yaml\simbackend-deployment.yaml"
kubectl apply -f "%BACKEND_ROOT%\yaml\simbackend-service.yaml"

echo Applying frontend manifests...
kubectl apply -f "%FRONTEND_ROOT%\yaml\simfrontend-deployment.yaml"
kubectl apply -f "%FRONTEND_ROOT%\yaml\simfrontend-service.yaml"

echo Applying gateway and routes...
kubectl apply -f "%BACKEND_ROOT%\yaml\sim-gateway.yaml"
kubectl apply -f "%BACKEND_ROOT%\yaml\backend-route.yaml"
kubectl apply -f "%FRONTEND_ROOT%\yaml\frontend-route.yaml"

echo.
echo Deployment finished.
pause
