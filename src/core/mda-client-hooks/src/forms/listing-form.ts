export async function OnLoad(context: Xrm.Events.EventContext): Promise<void> {
    const formContext = context.getFormContext();
    console.log('OnLoad hook' + formContext.data.entity.getEntityName());
}
