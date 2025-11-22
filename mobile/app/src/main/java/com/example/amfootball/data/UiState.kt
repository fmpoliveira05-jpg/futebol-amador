package com.example.amfootball.data

/**
 * Data class que determina o estado da Ui (User Interface).
 *
 * É o modelo padrão utilizado para comunicar o status de Carregamento e Erro
 * entre a ViewModel e a Composable, seguindo o padrão Loading/Error.
 *
 * @property isLoading Flag booleana que indica se uma operação de dados está em andamento.
 * @property errorMessage A mensagem de erro a ser exibida. É null se a página estiver OK ou carregada.
 */
data class UiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
)
