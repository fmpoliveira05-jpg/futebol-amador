package com.example.amfootball.domains.enums

import com.google.gson.Gson
import org.junit.Assert.assertEquals
import org.junit.Test

/**
 * A app envia a posição como `ordinal` e a API guarda-a no enum `Position` do backend
 * (FORWARD = 0, MIDFIELDER = 1, DEFENDER = 2, GOALKEEPER = 3). Se a ordem mudar, os jogadores
 * ficam registados na posição errada, como acontecia antes desta correção.
 */
class PositionTest {

    @Test
    fun ordinaisCoincidemComOBackend() {
        assertEquals(0, Position.FORWARD.ordinal)
        assertEquals(1, Position.MIDFIELDER.ordinal)
        assertEquals(2, Position.DEFENDER.ordinal)
        assertEquals(3, Position.GOALKEEPER.ordinal)
    }

    @Test
    fun desserializaONumeroEnviadoPelaApi() {
        val gson = Gson()
        // A API manda o enum como número (0, 1, ...); o Gson lê-o como texto e usa o @SerializedName.
        assertEquals(Position.FORWARD, gson.fromJson("0", Position::class.java))
        assertEquals(Position.GOALKEEPER, gson.fromJson("3", Position::class.java))
    }

    @Test
    fun desserializaONomeEmIngles() {
        val gson = Gson()
        assertEquals(Position.MIDFIELDER, gson.fromJson("\"Midfielder\"", Position::class.java))
        assertEquals(Position.GOALKEEPER, gson.fromJson("\"GoalKeeper\"", Position::class.java))
    }
}
