package com.example.amfootball.data.dtos.membershipRequest

/**
 * Data Transfer Object (DTO) que representa a informação básica da entidade que recebe
 * um pedido ou convite (o Recetor).
 *
 * É tipicamente usado dentro do [MembershipRequestInfoDto].
 *
 * @property id O identificador único do recetor (ex: ID da Equipa).
 * @property name O nome da entidade recetora (ex: Nome da Equipa).
 */
data class ReceiverInfo(
    val id: String,
    val name: String,
)
