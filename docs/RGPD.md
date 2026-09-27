# Proteção de dados (RGPD)

Como o Futebol Amador trata dados pessoais: registo das atividades de tratamento (art. 30.º), fundamentos,
prazos, subcontratantes, pedidos dos titulares e violações de dados. O texto para os utilizadores está na
página `/privacidade` da web e no ecrã "Política de Privacidade" da app Android (versão `2026-09-27`).

## Antes de publicar

- Indicar o responsável pelo tratamento e o contacto em `contactoPrivacidade` (`web/src/app/environments/*.ts`;
  hoje é o marcador `privacidade@futebol-amador.example`) e no texto da app.
- Assinar/aceitar os acordos de tratamento de dados (DPA) da Google (Firebase), Cloudinary, Cloudflare e do
  alojamento.
- Ativar a isenção de índice do Firestore `messages.senderId` com âmbito *grupo de coleções* (necessária à
  exportação e ao apagamento das mensagens de um utilizador).

## Registo das atividades de tratamento (art. 30.º)

| Tratamento | Dados | Titulares | Finalidade | Fundamento | Prazo |
|---|---|---|---|---|---|
| Conta e autenticação | nome, e-mail, palavra-passe (só no Firebase), telefone, morada, data de nascimento | utilizadores | criar e gerir a conta | contrato (6.º/1/b) | enquanto a conta existir; contas não confirmadas: 30 dias |
| Perfil desportivo | posição, altura, peso, pé, nacionalidade, país de nascimento, fotografia | utilizadores | equipas, mercado, onzes | contrato | enquanto a conta existir |
| Equipas, jogos e ligas | equipa, pedidos, convites, transferências, onzes, eventos | utilizadores | organizar jogos e ligas | contrato | pedidos: 90 dias; convites: 30 dias após a data; estatísticas: anonimizadas ao eliminar a conta |
| Chat | mensagens, membros das salas | utilizadores da mesma equipa | comunicação da equipa | contrato | até à eliminação da conta |
| Notificações | token FCM | utilizadores que autorizam | avisos de jogos e convites | contrato | até terminar a sessão/eliminar a conta |
| Segurança | IP (limitação de pedidos, em memória), Turnstile | visitantes e utilizadores | prevenir abusos | interesse legítimo (6.º/1/f) | não guardado |
| Localização | coordenadas aproximadas (só no aparelho, para preencher a morada) | quem toca em "usar a minha localização" | conveniência no registo | consentimento (permissão do Android) | não enviada à API |
| Cópias de segurança | tudo o acima | todos | recuperação | interesse legítimo | SQL 14 dias; Firestore 90 dias |

Destinatários: os outros utilizadores veem nome, posição, localidade, equipa e estatísticas; e-mail, telefone,
morada e data de nascimento só o próprio e os colegas de equipa (`PlayerDetailsDto.OcultarDadosPessoais`).

## Subcontratantes

| Quem | Para quê | Localização / transferências |
|---|---|---|
| Google (Firebase Authentication, Firestore, Cloud Messaging) | autenticação, chat, notificações | UE/EUA; Quadro de Privacidade de Dados UE-EUA e cláusulas contratuais-tipo |
| Cloudinary | emblemas e fotografias | configurar a região; cláusulas contratuais-tipo |
| Cloudflare (Turnstile) | verificação anti-robô na web | global; DPA da Cloudflare |
| Alojamento da API e do SQL Server | execução do serviço | escolher região na UE |

A web não carrega tipos de letra nem scripts de terceiros (verificado: sem Google Fonts nem CDN); o único recurso
externo é o Turnstile nos formulários de entrada e registo, quando configurado.

## Direitos dos titulares

| Direito | Como |
|---|---|
| Informação (13.º) | `/privacidade` (web, também no rodapé e no registo) e ecrã na app, acessível antes do registo |
| Consentimento/aceitação | caixa obrigatória no registo (web e Android); a API recusa sem a versão atual e guarda `PoliticaPrivacidadeVersao` e `PoliticaPrivacidadeAceiteEm` (migração `ConsentimentoEEliminacaoRgpd`) |
| Acesso e portabilidade (15.º, 20.º) | `GET /api/User/me/export` → ficheiro JSON; botão "Descarregar os meus dados" na web e na app |
| Retificação (16.º) | edição do perfil |
| Apagamento (17.º) | `DELETE /api/User/me` com a palavra-passe; botão na web e na app |
| Oposição/limitação | pelo contacto; resposta em 1 mês |

**Eliminação da conta** (`ContaService.EliminarContaAsync`):

1. confirma a palavra-passe no Firebase (uma sessão roubada não chega);
2. sai da equipa com as regras normais: a administração e o estatuto de administrador principal passam ao membro
   seguinte; se era o único membro, a equipa é apagada (falha se a equipa tiver jogos marcados que impeçam apagá-la —
   a mensagem diz o que fazer);
3. apaga pedidos de adesão, a colocação no mercado e as propostas em aberto;
4. **anonimiza** o jogador ("Jogador removido", e-mail/telefone/morada/data de nascimento substituídos, sem
   fotografia nem token), mantendo a linha para as estatísticas dos jogos já disputados (interesse legítimo das
   outras equipas; ninguém fica identificável) e marca `EliminadoEm` (deixa de aparecer nas listas);
5. apaga as mensagens do chat enviadas pelo titular e retira-o das salas (Firestore);
6. apaga as imagens da pasta `equipas/{uid}` no Cloudinary que já não são emblema de nenhuma equipa;
7. apaga o utilizador no Firebase Authentication e os cookies da sessão.

Se os passos 5 ou 6 falharem, a eliminação continua e fica um erro no log ("Repetir manualmente") para seguimento.
O antigo `DELETE /api/Player/{id}` (sem palavra-passe) responde `410`.

## Retenção automática

`RetencaoDadosBackGroundService` (uma vez por dia; `Rgpd:RetencaoAtiva`):

- contas sem equipa criadas há mais de `Rgpd:DiasContaPorConfirmar` (30) dias cujo e-mail nunca foi confirmado no
  Firebase → apagadas (base de dados e Firebase);
- pedidos de adesão com mais de `Rgpd:DiasPedidosAdesao` (90) dias → apagados;
- convites de jogo com data há mais de `Rgpd:DiasConvitesExpirados` (30) dias → apagados.

## Cookies e armazenamento

Só estritamente necessários, por isso sem banner de consentimento (art. 5.º/3 da Diretiva ePrivacy):

| Nome | Tipo | Conteúdo | Duração |
|---|---|---|---|
| `__Host-fa_session` | cookie `HttpOnly; Secure; SameSite=Strict` | ID token do Firebase | 1 hora |
| `__Secure-fa_refresh` | cookie `HttpOnly; Secure; SameSite=Strict`, `Path=/api/User/refresh` | refresh token | 7 dias |
| `fa_sessao` | `localStorage` | id interno da conta, id da equipa, se é administrador (sem dados pessoais nem tokens) | até terminar a sessão |

O `sessionStorage` só é usado pelo modo demonstração (dados fictícios); o browser apaga-o ao fechar o separador.
Teste automático: `web/e2e/armazenamento.spec.ts` (build de produção) confirma que o armazenamento não tem JWT,
tokens, e-mail, nome, telefone, morada nem data de nascimento, que os cookies são `HttpOnly`/`Secure`/`Strict`
e que o logout apaga cookies e armazenamento.

Android: token e perfil cifrados com AES-256-GCM e chave do Android Keystore (`CifraLocal`/`CifraAesGcm`, com testes
unitários), sem cópias de segurança (`data_extraction_rules.xml`). Permissões: notificações; calendário (sincronizar
os jogos no calendário do telemóvel, pedida no arranque — justificação: funcionalidade pedida pelo utilizador;
pode ser negada); câmara (fotografia do emblema, pedida ao usar); localização **aproximada** só ao tocar em
"usar a minha localização" no registo (a localização exata e o pedido no arranque foram retirados).

## Violação de dados

1. Conter (revogar chaves, `firebase auth` desativar contas afetadas, rodar segredos, repor cópia).
2. Avaliar risco (que dados, quantos titulares, cifrados ou não). Registar tudo (art. 33.º/5), mesmo sem notificação.
3. Se houver risco: notificar a **CNPD em 72 horas** (formulário em www.cnpd.pt), mesmo com informação incompleta.
4. Se o risco for elevado: avisar os titulares sem demora (e-mail), com o que aconteceu, as consequências e o que
   fazer (mudar palavra-passe).
5. Rever as causas e atualizar este documento e o SEGURANCA.md.

## Pedidos dos titulares

Registar a data, verificar a identidade (sessão iniciada ou e-mail da conta), responder em 1 mês (prorrogável por
2 meses em casos complexos). A exportação e a eliminação são self-service; a limitação e a oposição fazem-se à mão.
