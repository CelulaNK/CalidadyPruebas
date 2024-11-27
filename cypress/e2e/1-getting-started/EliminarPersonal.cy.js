describe('eliminar', () => {

    it('Eliminar Personal', () => {

        cy.visit('http://localhost:1287/Empleados/Login');
      cy.get('input[name="ID"]')
      .type('ADMINMax');
  
      cy.get('input[name="Contraseña"]')
      .type('1234');
  
      cy.get('form').submit();
  
      cy.get('#header a[href="#menu"]').click(); 
      cy.get('#menu.visible', { timeout: 1000 }).should('be.visible');
      cy.contains('a', 'Listar Personal').click();
      cy.contains('tr','usuarioo').should('exist')
      cy.contains('a', 'Delete').click();
      cy.get('form').submit();

      
    })
  })