package com.example.amfootball.data.dtos.player

import android.net.Uri
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

/**
 * Data Transfer Object (DTO) que representa os detalhes completos de um membro de equipa
 * (seja jogador, administrador ou outro tipo de staff).
 *
 * Utilizado para listagens e gestão de membros dentro do contexto da equipa.
 *
 * @property id O identificador único do membro.
 * @property name O nome completo do membro.
 * @property typeMember O tipo de função do membro na equipa (Enum [TypeMember]).
 * @property image A URI da imagem de perfil do membro.
 * @property age A idade do membro.
 * @property position A posição principal do membro em campo (Enum [Position]).
 * @property size A altura do membro em centímetros.
 */
data class MemberTeamDto(
    val id: String = "",
    val name: String = "",
    val typeMember: TypeMember = TypeMember.PLAYER,
    val image: Uri = Uri.EMPTY,
    val age: Int = 0,
    val position: Position = Position.MIDFIELDER,
    val size: Double = 0.0
) {
    companion object{
        fun createExampleList(): List<MemberTeamDto> {
            val players = mutableListOf<MemberTeamDto>()
            val positions = Position.values()

            for (i in 1..32) {
                val playerPosition = when (i) {
                    1, 22, 32 -> Position.GOALKEEPER
                    in 2..9 -> Position.DEFENDER
                    in 10..21 -> Position.MIDFIELDER
                    else -> Position.FORWARD
                }

                if(i < 28) {
                    players.add(
                        MemberTeamDto(
                            id = "player_$i",
                            name = "Jogador $i",
                            typeMember = TypeMember.PLAYER,
                            image = Uri.EMPTY,
                            age = (18..35).random(),
                            position = playerPosition,
                            size = (165..195).random()
                        )
                    )
                } else {
                    players.add(
                        MemberTeamDto(
                            id = "player_$i",
                            name = "Jogador $i",
                            typeMember = TypeMember.ADMIN_TEAM,
                            image = Uri.EMPTY,
                            age = (18..35).random(),
                            position = playerPosition,
                            size = (165..195).random()
                        )
                    )
                }
            }
            return players
        }
    }
}
