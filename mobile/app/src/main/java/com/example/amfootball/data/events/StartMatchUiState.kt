package com.example.amfootball.data.events

/**
 * Representa os possíveis estados da UI durante o processo de inicialização de uma partida,
 * especialmente em ambientes que requerem sincronização (e.g., conexões em tempo real ou sockets).
 *
 * Sendo uma `sealed class`, garante que todos os estados são tratados no código que a consome
 * (por exemplo, numa expressão `when`).
 */
sealed class StartMatchUiState{
    /**
     * Estado de carregamento genérico.
     * Tipicamente exibido enquanto se espera a resposta inicial do servidor ou a preparação de dados.
     */
    object Loading : StartMatchUiState()

    /**
     * Estado indicando que o cliente está a tentar estabelecer uma conexão (ex: socket) com o servidor.
     */
    object Connecting : StartMatchUiState()

    /**
     * Estado indicando que a conexão foi estabelecida, mas o sistema está à espera
     * que o adversário confirme a sua prontidão para iniciar o jogo.
     */
    object WaitingForOpponent : StartMatchUiState()

    /**
     * Estado final e bem-sucedido. O jogo foi iniciado e a navegação para o ecrã do jogo
     * ou gestão em tempo real deve ocorrer agora.
     */
    object MatchStarted : StartMatchUiState()

    /**
     * Estado indicando que o processo de inicialização foi cancelado, seja pelo utilizador
     * ou por um timeout/falha na sincronização.
     */
    object Cancelled : StartMatchUiState()

    /**
     * Estado de erro, contendo uma mensagem detalhada para feedback ao utilizador.
     *
     * @param msg A mensagem de erro localizada a ser exibida.
     */
    data class Error(val msg: String) : StartMatchUiState()
}