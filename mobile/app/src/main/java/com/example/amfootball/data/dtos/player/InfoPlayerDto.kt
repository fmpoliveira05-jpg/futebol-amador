package com.example.amfootball.data.dtos.player

import com.example.amfootball.data.enums.Position

/**
 * Data Transfer Object (DTO) que representa as informações essenciais de um jogador.
 *
 * Utilizado para listas e visualizações de perfil onde um resumo dos dados do jogador
 * (idade, posição, e status da equipa) é necessário.
 *
 * @property id O identificador único do jogador.
 * @property name O nome completo do jogador.
 * @property image A URL ou caminho da imagem de perfil do jogador (pode ser null).
 * @property address O endereço ou cidade de residência do jogador.
 * @property age A idade do jogador.
 * @property position A posição principal do jogador em campo (Enum [Position]).
 * @property heigth A altura do jogador em centímetros.
 * @property haveTeam Flag que indica se o jogador está atualmente associado a uma equipa.
 */
data class InfoPlayerDto(
    val id: String = "",
    val name: String = "",
    val image: String? = null,
    val address: String = "",
    val age: Int = 0,
    val position: Position,
    val heigth: Int = 0,
    val haveTeam: Boolean = false,
) {
    /*
    * companion object {
        fun createExamplePlayerList(): List<InfoPlayerDto> {
            val firstNames = listOf(
                "Diogo", "Bruno", "João", "Rui", "Cristiano", "Pedro",
                "Miguel", "Nuno", "André", "Fábio"
            )
            val lastNames = listOf(
                "Silva", "Costa", "Fernandes", "Santos", "Gomes",
                "Lopes", "Alves", "Martins", "Dias", "Ribeiro"
            )

            val cities = listOf("Lisboa", "Porto", "Braga", "Faro", "Coimbra", "Aveiro")

            val allPositions = Position.values()

            return (1..20).map {
                InfoPlayerDto(
                    id = UUID.randomUUID().toString(),
                    name = "${firstNames.random()} ${lastNames.random()}",
                    address = cities.random(),
                    age = (18..38).random(),
                    position = allPositions.random(),
                    size = (165..195).random()
                )
            }
        }
    }
    * */
}
