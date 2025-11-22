package com.example.amfootball.data.actions.filters

/**
 * Data class que agrupa todas as funções de callback (ações) necessárias para manipular
 * o estado de filtragem num ecrã de pedidos de adesão de membros a uma equipa.
 *
 * @property onSenderNameChange Callback para atualizar o filtro por nome do remetente (quem enviou o pedido).
 * @property onMinDateSelected Callback para atualizar a data mínima de envio/receção do pedido no filtro (em milissegundos).
 * @property onMaxDateSelected Callback para atualizar a data máxima de envio/receção do pedido no filtro (em milissegundos).
 * @property buttonActions Objeto que contém os callbacks para os botões de controlo (Aplicar e Limpar).
 */
data class FilterMemberShipRequestActions(
    val onSenderNameChange: (newName: String) -> Unit,
    val onMinDateSelected: (newMinDate: Long) -> Unit,
    val onMaxDateSelected: (newMaxDate: Long) -> Unit,
    val buttonActions: ButtonFilterActions
)
