import React from 'react'

export const ChatInfo = ({name} : {name: string}) => {
  return (
    <div className='chat-container'>
        <div>
            <h1>{name? `${name}`: ''}</h1>
        </div>
    </div>
  )
}
