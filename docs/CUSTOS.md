# Custos e limites de gastos

Estimativas e travões para os serviços pagos (ou com camada gratuita) usados pelo Futebol Amador. Os valores dos
planos mudam: confirmar nas páginas de preços antes de publicar.

## Resumo

| Serviço | Uso | Camada gratuita | Travões |
|---|---|---|---|
| Firebase Authentication | contas e-mail/palavra-passe | gratuito para e-mail/palavra-passe (Identity Platform cobra acima de ~50 000 MAU) | quotas de registo por IP na API; **SMS não é usado** (não ativar o fornecedor "Telefone") |
| Firebase e-mails (confirmação, recuperação) | `sendOobCode` | incluído (limites diários do Firebase) | 5 pedidos / 15 min por IP (`LimitacaoPedidos:Email`); resposta sempre igual |
| Cloud Firestore | chat e cache da classificação | 50 000 leituras, 20 000 escritas, 1 GiB por dia (plano Spark) | regras só para membros; mensagens até 1000 caracteres; salas só pela API (10 por utilizador/dia) |
| Cloud Messaging (FCM) | notificações | gratuito | — |
| Cloudinary | emblemas e fotografias | plano Free (créditos mensais de armazenamento, transformações e largura de banda) | uploads assinados pela API (20 por utilizador/hora), JPG/PNG/WebP, 2 MB no cliente, `c_limit,w_512,h_512` no preset |
| Cloudflare Turnstile | anti-robô na web | gratuito | — |
| Alojamento da API | contentor .NET | depende do fornecedor | limites de pedidos globais e por utilizador |
| SQL Server | base de dados | SQL Server Express (10 GB) ou Azure SQL serverless | índices; paginação (máx. 100); cache de 60 s nas consultas públicas |
| GitHub Actions | CI e `uptime.yml` | gratuito em repositórios públicos | o monitor corre a cada 15 min; nada acontece sem `FA_HEALTH_URL` |

## Firebase (plano Blaze)

O Firestore, o FCM e as exportações do Firestore (cópias de segurança) precisam do plano Blaze (pagamento conforme o
uso). Travões:

1. **Alertas de orçamento** na Google Cloud: *Billing → Budgets & alerts → Create budget*, por exemplo 10 €/mês
   com alertas a 50 %, 90 % e 100 % (e-mail). O orçamento **avisa mas não corta** o serviço.
2. Corte automático (opcional): alerta de orçamento para um tópico Pub/Sub e uma Cloud Function que desliga a
   faturação do projeto ao chegar ao limite (exemplo oficial "Cap (disable) billing to stop usage").
3. Quotas: *APIs & Services → Quotas* do Identity Toolkit (registos por IP, e-mails por dia) — baixar os limites para
   o necessário.
4. App Check (trabalho futuro, ver SEGURANCA.md) reduz o abuso do Firestore por clientes falsos.
5. Exportações do Firestore: cada exportação cobra as leituras de todos os documentos; com a retenção de 90 dias no
   bucket, o armazenamento fica limitado.

## Cloudinary

- Plano Free: acompanhar os créditos em *Dashboard → Usage*; ativar os alertas de utilização por e-mail.
- O preset assinado limita formatos e redimensiona à entrada; a API só assina uploads para a pasta do utilizador e
  com quota por utilizador (`LimitacaoPedidos:Uploads`, 20/hora). Imagens de contas eliminadas são apagadas.
- Limite de tamanho: o Cloudinary aceita até ao limite do plano (10 MB por imagem no Free); a app Android
  limita a 2 MB antes de enviar; a web aceita até 10 MB no browser, redimensiona para 256 × 256 px e envia em
  base64 (a API aceita até ~150 KB).

## Alojamento e SQL Server

- API: 1 instância pequena (1 vCPU, 1–2 GB) chega para centenas de utilizadores simultâneos nas consultas públicas
  (ver o teste de carga em OPERACAO.md: ~1500 pedidos/s em 2 vCPU partilhados).
- Azure App Service/Container Apps ou uma VPS: definir alertas de custo do fornecedor.
- SQL Server: Express é gratuito até 10 GB por base de dados; no Azure SQL, o nível *serverless* com pausa
  automática e um orçamento mensal no *Cost Management*.
- Cloudflare (se usado à frente da web/API): plano Free; *rate limiting rules* do Cloudflare como segunda barreira.

## Limites no código

| Operação | Limite | Chave |
|---|---|---|
| Todos os pedidos por IP | 300/min | `LimitacaoPedidos:Global` |
| Login | 10 / 5 min por IP | `LimitacaoPedidos:Autenticacao` |
| Registo | 5 / hora por IP | `LimitacaoPedidos:Registo` |
| E-mails (confirmação, recuperação) | 5 / 15 min por IP | `LimitacaoPedidos:Email` |
| Assinaturas de upload | 20 / hora por utilizador | `LimitacaoPedidos:Uploads` |
| Salas de chat | 10 / dia por utilizador | `LimitacaoPedidos:SalasChat` |
| Convites, pedidos de adesão, propostas | 30 / hora por utilizador | `LimitacaoPedidos:Convites` |
| Equipas criadas | 3 / dia por utilizador | `LimitacaoPedidos:Equipas` |
| Exportações de dados | 5 / hora por utilizador | `LimitacaoPedidos:Exportacao` |
| Pedidos a serviços externos em curso | 200 | `Limites.AddResilienciaExterna` |

Os e-mails do Firebase por utilizador estão limitados pela quota por IP e pelo próprio Firebase (que recusa envios
repetidos para o mesmo endereço em pouco tempo); não há outro envio de e-mails na API.
