package com.example.amfootball.data.enums.Forms

/**
 * Define o modo de operação (comportamento) do formulário de gestão de Jogos.
 *
 * Este enum é utilizado para configurar a Interface do Utilizador (UI) do ecrã de detalhes/edição de jogo,
 * determinando quais os campos que devem estar visíveis ou editáveis, e qual a ação a ser executada
 * ao submeter o formulário (criar, alterar ou cancelar).
 */
enum class MatchFormMode {
    /**
     * Modo de criação/envio.
     * Utilizado quando se pretende enviar um novo convite de jogo a uma equipa adversária.
     * Todos os campos são geralmente editáveis.
     */
    SEND,

    /**
     * Modo de negociação.
     * Utilizado quando se responde a um convite existente com uma contra-proposta
     * (ex: aceitar o jogo mas alterar a hora ou o local).
     */
    NEGOCIATE,

    /**
     * Modo de cancelamento.
     * Utilizado para anular um jogo agendado ou retirar um convite enviado anteriormente.
     * Geralmente torna os campos "apenas leitura" e foca-se na confirmação da ação.
     */
    CANCEL,

    /**
     * Modo de adiamento.
     * Utilizado especificamente para propor uma nova data para um jogo já confirmado (reagendamento),
     * sem alterar necessariamente outros detalhes como o local.
     */
    POSTPONE
}