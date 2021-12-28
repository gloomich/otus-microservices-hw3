Deployment:

- Create namespace and set as default
```
kubectl create namespace hw3
kubectl config set-context --current --namespace hw3
```

- Add and update helm repositories
```
helm repo add stable https://charts.helm.sh/stable
helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo add ingress-nginx https://kubernetes.github.io/ingress-nginx
helm repo update
```

- Install prometheus
```
helm install prom prometheus-community/kube-prometheus-stack -f ./prom/prometheus.yaml --atomic
```

- Install ingress
```
helm install nginx ingress-nginx/ingress-nginx -f nginx-ingress.yaml --atomic
```

- Install pg
```
helm install pg -f pg-values.yaml bitnami/postgresql --atomic
```

- Install app
```
helm install hw3 ./otus-microsrv-hw3
```

- Open prometheus port
```
kubectl port-forward service/prom-kube-prometheus-stack-prometheus 9090
```

- Open grafana port (admin:prom-operator)
```
kubectl port-forward service/prom-grafana 9000:80
```

- Add dashboard
```
kubectl apply -f grafana.yaml
```

Swagger URL: http://arch.homework/swagger/

Postman collection for tests: ./tests/hw2-postman-collection.json

JMeter load tests: Users.jmx

---

Домашнее задание разработано для курса ["Microservice Architecture"](https://otus.ru/lessons/microservice-architecture)
