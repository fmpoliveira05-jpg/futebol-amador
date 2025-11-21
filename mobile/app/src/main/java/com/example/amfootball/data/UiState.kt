package com.example.amfootball.data

/**
 * Data class que determina o estado da Ui, se a pagina está em loading,
 * se está com mensagem de erro. Ou se não estiver com nenhum dos dois com os dados carregados
 * */
data class UiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
)
