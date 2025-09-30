# Guia de implantação no Kubernetes (Docker Desktop)

Este diretório contém os manifestos necessários para executar o MVP da Visionary Analytics no cluster Kubernetes que acompanha o Docker Desktop. Os arquivos reproduzem o mesmo comportamento do `docker-compose.yml`, mas separados em recursos próprios do Kubernetes.

## 1. Pré-requisitos
- Windows 10/11 ou macOS com **Docker Desktop** instalado.
- Suporte ao Kubernetes habilitado no Docker Desktop (Settings ➜ Kubernetes ➜ *Enable Kubernetes*).
- `kubectl` configurado para apontar para o contexto `docker-desktop` (padrão quando o Kubernetes do Docker Desktop está ativo).

## 2. Construir as imagens locais
O cluster do Docker Desktop consegue ler imagens presentes no cache local. Gere as imagens usadas pelos manifestos com os mesmos nomes definidos no `docker-compose.yml`:

```powershell
# Na raiz do repositório
$env:DOCKER_BUILDKIT=1

docker build -t visionaryanalytics-api:latest --target api .
docker build -t visionaryanalytics-frameworker:latest --target frameworker .
docker build -t visionaryanalytics-videoworker:latest --target videoworker .
```

## 3. Aplicar os manifestos
Aplique os recursos na ordem abaixo para criar namespace, volumes, segredos e implantações:

```bash
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/secrets.yaml
kubectl apply -f kubernetes/configmap.yaml
kubectl apply -f kubernetes/persistent-volumes.yaml
kubectl apply -f kubernetes/rabbitmq.yaml
kubectl apply -f kubernetes/mongo.yaml
kubectl apply -f kubernetes/api.yaml
kubectl apply -f kubernetes/frame-worker.yaml
kubectl apply -f kubernetes/video-worker.yaml
```

Verifique o status dos pods:

```bash
kubectl get pods -n visionary-analytics
```

## 4. Acessar serviços expostos
- **API**: disponível em `http://localhost:30080` (Service `NodePort`).
- **RabbitMQ Management**: painel em `http://localhost:31567` (usuário/senha `guest`).
- **MongoDB**: porta `32017` exposta para conexões externas (ex.: `mongodb://mongoadmin:secret@localhost:32017/`).

## 5. Volumes persistentes
- `pvc-video-storage`: armazena o vídeo enviado e os frames gerados. Montado em `/app/uploads` para API e workers.
- `pvc-mongo-dados`: mantém os dados do MongoDB entre recriações do pod.

Para inspecionar as `PersistentVolumeClaims` e seus volumes:

```bash
kubectl get pvc -n visionary-analytics
```

## 6. Encerramento e limpeza
Para remover todos os recursos criados:

```bash
kubectl delete -f kubernetes/video-worker.yaml \
  -f kubernetes/frame-worker.yaml \
  -f kubernetes/api.yaml \
  -f kubernetes/mongo.yaml \
  -f kubernetes/rabbitmq.yaml \
  -f kubernetes/persistent-volumes.yaml \
  -f kubernetes/configmap.yaml \
  -f kubernetes/secrets.yaml \
  -f kubernetes/namespace.yaml
```

> **Observação:** os volumes persistentes são excluídos automaticamente ao remover o namespace. Caso queira preservar dados entre execuções, pule a exclusão do arquivo `namespace.yaml` e apenas remova os deployments.

## 7. Fluxo de testes
O fluxo manual é idêntico ao descrito para Docker Compose. Utilize a URL da API (`http://localhost:30080`) para enviar vídeos, consultar status/resultados e acompanhar as notificações SignalR (`/hubs/processamento`).
