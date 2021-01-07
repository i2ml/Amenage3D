
// DO NOT FORGET TO ADD A REFERENCE OF YOUR GUIDE FILE IN THE INDEX.HTML FILE
$(function() {
    registerGuide({ 
        name: 'Quick-Start', // THE NAME OF THE PAGE
        summary: 'This is just an example of guide page', // A SUMMARY DISPLAYED IN THE GUIDES PAGES
        content: `
            <h1 class='page-heading'>Quick Start</h1>

            <section>
                <span class=‘section-heading'>What is Lorem Ipsum?</span>
                <p>
                    Lorem Ipsum is simply dummy text of the printing and typesetting industry. 
                    Lorem Ipsum has been the industry's standard dummy text ever since the 1500s,
                    when an unknown printer took a galley of type and scrambled it to make a
                    type specimen book. It has survived not only five centuries, but also the 
                    leap into electronic typesetting, remaining essentially unchanged.
                    It was popularised in the 1960s with the release of Letraset sheets containing 
                    Lorem Ipsum passages, and more recently with desktop publishing software 
                    like Aldus PageMaker including versions of Lorem Ipsum.
                </p>
            </section>

            <section>
             <span class=‘section-heading'>What is Lorem Ipsum?</span>
            <p>
        
                Lorem Ipsum is simply dummy text of the printing and typesetting industry. 
                Lorem Ipsum has been the industry's standard dummy text ever since the 1500s,
                when an unknown printer took a galley of type and scrambled it to make a
                type specimen book. It has survived not only five centuries, but also the 
                leap into electronic typesetting, remaining essentially unchanged.
                It was popularised in the 1960s with the release of Letraset sheets containing 
                Lorem Ipsum passages, and more recently with desktop publishing software 
                like Aldus PageMaker including versions of Lorem Ipsum.

                <see cref='TestClass.Property'/>
            </p>
            <code>
                using System;

                namespace TestNamespace {
                    public class A {
                        void Test() {
                            
                        }
                    }

                }
            </code>
        </section>
        `
    });
});