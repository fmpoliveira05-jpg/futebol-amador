package com.example.amfootball.data.dtos.membershipRequest

import android.net.Uri

/**
 * Data Transfer Object (DTO) que representa a informação básica da entidade que envia
 * um pedido ou convite (o Remetente).
 *
 * É tipicamente usado dentro do [MembershipRequestInfoDto].
 *
 * @property id O identificador único do remetente (ex: ID do Jogador).
 * @property name O nome da entidade remetente (ex: Nome do Jogador).
 * @property image A URI da imagem de perfil ou logótipo do remetente.
 */
data class SenderInfo(
    val id: String,
    val name: String,
    val image: Uri,
)