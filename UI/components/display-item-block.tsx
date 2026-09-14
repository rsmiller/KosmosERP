

function DisplayItemBlock({label, value}: any) {

    return (
        <div className={"display-item-block"}> 
            <div className={"display-item-block-label"}>{label}:</div>
            <div className={"display-item-block-value"}>{value}</div>
        </div>
    )
}

export default DisplayItemBlock;