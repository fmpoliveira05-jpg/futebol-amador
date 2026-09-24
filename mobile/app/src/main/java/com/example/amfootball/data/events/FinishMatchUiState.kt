package com.example.amfootball.data.events

/**
 * Representa os estados possíveis da interface de utilizador (UI State) durante o processo
 * de finalização, reporte e confirmação do resultado de uma partida (Finish Match Screen).
 *
 * Sendo uma `sealed class`, garante que todos os estados possíveis são conhecidos e tratados,
 * tornando a lógica da UI exaustiva e segura.
 */
sealed class FinishMatchUiState {

    /**
     * Estado inicial de carregamento de dados essenciais ou estado genérico de atividade.
     *
     * Indica que o ecrã está a preparar-se ou a aguardar a conclusão de uma operação.
     */
    object Loading : FinishMatchUiState()

    /**
     * Estado específico de conexão com o serviço em tempo real (SignalR).
     *
     * Indica que o cliente está a tentar estabelecer a comunicação com o Hub do servidor.
     */
    object Connecting : FinishMatchUiState()

    /**
     * Estado após a submissão do resultado, onde o sistema está a aguardar a confirmação
     * por parte do adversário ou do servidor (normalmente via SignalR).
     *
     * O utilizador deve ser notificado de que o processo está pendente.
     */
    object WaitingForConfirmation : FinishMatchUiState()

    /**
     * Estado final de sucesso.
     *
     * Indica que o resultado da partida foi validado e registado com sucesso por ambas as partes.
     */
    object MatchFinished : FinishMatchUiState()

    /**
     * Estado intermédio onde o utilizador está a alterar ou reenviar um resultado.
     *
     * Usado quando uma submissão anterior falhou ou precisa ser corrigida.
     */
    object Editing : FinishMatchUiState()

    /**
     * Estado de erro.
     *
     * Indica que ocorreu um problema irrecuperável ou uma falha na validação/rede.
     * @property msg Mensagem de erro descritiva a ser exibida ao utilizador.
     */
    data class Error(val msg: String) : FinishMatchUiState()
}