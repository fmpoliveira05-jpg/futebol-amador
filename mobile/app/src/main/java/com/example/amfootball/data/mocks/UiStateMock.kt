package com.example.amfootball.data.mocks

import com.example.amfootball.data.UiState

/**
 * Fornece estados base fictícios (Mocks) para o wrapper genérico de estado da UI [UiState].
 *
 * Este objeto é utilizado para configurar o estado inicial de componentes em testes de UI (Previews)
 * e testes unitários, evitando a necessidade de instanciar manualmente o objeto [UiState] repetidamente.
 */
object UiStateMock {

    /**
     * Representa um estado de UI "Estável" ou de "Sucesso" (Content State).
     *
     * **Configuração:**
     * - `isLoading = false`: O carregamento terminou ou ainda não iniciou.
     * - `errorMessage = null`: Não existem erros para exibir.
     *
     * Este mock é ideal para envolver dados de lista (como os definidos noutros mocks)
     * e simular a visualização do ecrã quando os dados foram carregados com sucesso.
     */
    val mockUiStateContent = UiState(
        isLoading = false,
        errorMessage = null
    )
}