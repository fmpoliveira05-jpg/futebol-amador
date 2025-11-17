package com.example.amfootball.ui.viewModel

import com.google.firebase.Firebase
import com.google.firebase.auth.auth
import android.util.Log

/*
class ChatViewModel {
    fun fetchMyChatRooms() {
        val db = Firebase.firestore
        val myUserId = Firebase.auth.currentUser?.uid ?: return

        db.collection("chatRooms")
            .whereArrayContains("members", myUserId)
            .get()
            .addOnSuccessListener { querySnapshot ->
                val rooms = querySnapshot.documents.map { doc ->
                    // Mapear o 'doc' para um data class 'ChatRoom'
                    // ex: doc.id, doc.getString("name"), doc.get("members")
                }
                // Atualizar o State do Compose com a lista de 'rooms'
            }
            .addOnFailureListener { e ->
                Log.e("Chat", "Erro ao listar salas", e)
            }
    }
}

 */