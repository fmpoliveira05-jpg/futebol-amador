package com.example.amfootball.data.filters

import java.time.LocalDateTime

/**
 * Data class que contém todos os critérios de filtro aplicáveis a uma lista de pedidos de adesão de membros (Membership Requests).
 *
 * Utilizado para persistir e transferir o estado de filtragem entre a UI e a camada de dados/lógica.
 * Todos os campos são opcionais, permitindo filtros parciais.
 *
 * @property senderName Filtro pelo nome da equipa ou jogador que enviou o pedido.
 * @property minDate Filtro pela data e hora mínima de envio/receção do pedido.
 * @property maxDate Filtro pela data e hora máxima de envio/receção do pedido.
 */
data class FilterMemberShipRequest(
    val senderName: String? = null,
    val minDate: LocalDateTime? = null,
    val maxDate: LocalDateTime? = null
)