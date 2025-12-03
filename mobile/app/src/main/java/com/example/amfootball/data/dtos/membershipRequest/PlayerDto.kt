package com.example.amfootball.data.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

/**
 * Objeto de Transferência de Dados (DTO) que representa a identificação básica de um Jogador.
 *
 * Este DTO é utilizado especificamente em contextos de pedidos de adesão (Membership Requests),
 * onde apenas a identidade (ID) e o nome de exibição são necessários, evitando o overhead de
 * carregar o perfil completo do jogador.
 *
 * A classe utiliza anotações Gson para garantir a compatibilidade na serialização/deserialização,
 * suportando variações na nomenclatura das chaves JSON vindas da API.
 *
 * @property id O identificador único do jogador.
 * A anotação [@SerializedName] configura o Gson para ler o campo JSON `"Id"`, mas aceita
 * alternativamente `"id"` (camelCase) como fallback.
 *
 * @property name O nome de exibição do jogador.
 * A anotação [@SerializedName] configura o Gson para ler o campo JSON `"Name"`, mas aceita
 * alternativamente `"name"` (camelCase) como fallback.
 */
data class PlayerDto(
    @SerializedName("Id", alternate = ["id"])
    val id: String,
    @SerializedName("Name", alternate = ["name"])
    val name: String
)
