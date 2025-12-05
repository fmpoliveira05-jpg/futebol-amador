package com.example.amfootball.data.actions.filters

/**
 * Data class que agrupa todas as funções de callback (ações) necessárias para manipular
 * o estado de filtragem num ecrã de listagem de partidas adiadas.
 *
 * É o objeto de dependência passado da ViewModel para o ecrã de UI do filtro.
 *
 * @property onOpponentNameChange Callback para atualizar o filtro por nome do adversário (String).
 * @property onIsHomeChange Callback para atualizar o filtro de localização: true (Casa), false (Fora), ou null (Ambos).
 * @property onMinDateGameChange Callback para atualizar a data mínima **original** de jogo no filtro (em milissegundos).
 * @property onMaxDateGameChange Callback para atualizar a data máxima **original** de jogo no filtro (em milissegundos).
 * @property onMinDatePostPoneChange Callback para atualizar a data mínima **de adiamento** (remarcação) no filtro (em milissegundos).
 * @property onMaxDatePostPoneChange Callback para atualizar a data máxima **de adiamento** (remarcação) no filtro (em milissegundos).
 * @property buttonFilterActions Objeto que contém os callbacks para os botões de controlo (Aplicar e Limpar).
 */
data class FilterListPostPoneMatchActions(
    val onOpponentNameChange: (String) -> Unit,
    val onIsHomeChange: (Boolean?) -> Unit,
    val onMinDateGameChange: (Long) -> Unit,
    val onMaxDateGameChange: (Long) -> Unit,
    val onMinDatePostPoneChange: (Long) -> Unit,
    val onMaxDatePostPoneChange: (Long) -> Unit,
    val buttonFilterActions: ButtonFilterActions
)