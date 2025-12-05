package com.example.amfootball.data

/**
 * Data class que encapsula o estado visual genérico de um ecrã (UI State).
 *
 * Serve como contrato de comunicação entre o ViewModel e a UI (Composable),
 * centralizando a gestão de carregamentos, erros e notificações.
 *
 * @property isLoading Indica se uma operação assíncrona (ex: chamada à API) está em curso. Se `true`, a UI deve exibir um indicador de progresso.
 * @property errorMessage Mensagem de erro persistente ou de bloqueio (ex: falha ao carregar dados iniciais). Geralmente usada para exibir um ecrã de "Erro" com botão de "Tentar Novamente". É `null` se o estado for válido.
 * @property toastMessage Mensagem efêmera (One-shot event) destinada a Toasts ou Snackbars (ex: "Sem internet", "Sucesso"). Deve ser "consumida" (redefinida para `null` via ViewModel) imediatamente após a exibição para evitar repetições na rotação de ecrã.
 */
data class UiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val toastMessage: String? = null
)